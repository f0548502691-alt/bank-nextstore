import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, ViewEncapsulation, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription, finalize } from 'rxjs';

import {
  BankBalance,
  BankBalanceFilterOptions,
  BankBalanceSummary,
} from './features/dashboard/models/bank-balance.model';
import { BankBalanceFilterForm } from './features/dashboard/models/bank-balance-filter.model';
import { BalanceTableComponent } from './features/dashboard/components/balance-table/balance-table.component';
import { BankBalancesApiService } from './features/dashboard/services/bank-balances-api.service';
import { ChartsPanelComponent } from './features/dashboard/components/charts-panel/charts-panel.component';
import { FilterPanelComponent } from './features/dashboard/components/filter-panel/filter-panel.component';
import { SortOption } from './features/dashboard/models/sort-option.model';
import { SummaryCardsComponent } from './features/dashboard/components/summary-cards/summary-cards.component';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    SummaryCardsComponent,
    ChartsPanelComponent,
    FilterPanelComponent,
    BalanceTableComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css',
  encapsulation: ViewEncapsulation.None,
})
export class App implements OnInit, OnDestroy {
  private readonly bankBalancesApi = inject(BankBalancesApiService);
  private searchDebounceHandle: ReturnType<typeof setTimeout> | null = null;
  private balancesSubscription: Subscription | null = null;
  private requestSequence = 0;

  protected readonly balances = signal<BankBalance[]>([]);
  protected readonly filterOptions = signal<BankBalanceFilterOptions>({
    banks: [],
    currencies: [],
    balanceTypes: [],
    statuses: [],
  });
  protected readonly summary = signal<BankBalanceSummary | null>(null);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly searchPending = signal(false);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(50);
  protected readonly totalPages = signal(0);
  protected readonly totalCount = signal(0);
  protected readonly hasPreviousPage = signal(false);
  protected readonly hasNextPage = signal(false);
  protected readonly pageSizeOptions = [25, 50, 100, 250, 500];
  protected readonly sortOptions: readonly SortOption[] = [
    { value: 'date', label: 'תאריך' },
    { value: 'bankName', label: 'בנק' },
    { value: 'accountNumber', label: 'מספר חשבון' },
    { value: 'balanceType', label: 'סוג יתרה' },
    { value: 'currency', label: 'מטבע' },
    { value: 'amount', label: 'סכום' },
    { value: 'status', label: 'סטטוס' },
  ];
  protected readonly currencyTotals = computed(() =>
    Object.entries(this.summary()?.totalAmountByCurrency ?? {})
  );

  protected filterModel: BankBalanceFilterForm = this.createEmptyFilters();

  ngOnInit(): void {
    this.restoreStateFromUrl();
    this.loadFilterOptions();
    this.loadBalances();
  }

  ngOnDestroy(): void {
    this.clearSearchDebounce();
    this.balancesSubscription?.unsubscribe();
  }

  protected loadBalances(bypassCache = false): void {
    const requestId = ++this.requestSequence;
    this.balancesSubscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);
    this.filterModel.page = this.page();
    this.filterModel.pageSize = this.pageSize();
    this.syncUrlState();

    this.balancesSubscription = this.bankBalancesApi
      .getBalances(this.filterModel, { bypassCache })
      .pipe(finalize(() => {
        if (requestId === this.requestSequence) {
          this.loading.set(false);
        }
      }))
      .subscribe({
        next: (response) => {
          if (requestId !== this.requestSequence) {
            return;
          }

          this.balances.set(response.items);
          this.summary.set(response.summary);
          this.page.set(response.page);
          this.pageSize.set(response.pageSize);
          this.totalPages.set(response.totalPages);
          this.totalCount.set(response.totalCount);
          this.hasPreviousPage.set(response.hasPreviousPage);
          this.hasNextPage.set(response.hasNextPage);
          this.filterModel.page = response.page;
          this.filterModel.pageSize = response.pageSize;
          this.syncUrlState();
        },
        error: (error: unknown) => {
          if (requestId !== this.requestSequence) {
            return;
          }

          this.balances.set([]);
          this.summary.set(null);
          this.totalPages.set(0);
          this.totalCount.set(0);
          this.hasPreviousPage.set(false);
          this.hasNextPage.set(false);
          this.error.set(error instanceof Error ? error.message : 'לא ניתן לטעון את נתוני היתרות כרגע. נסו שוב מאוחר יותר.');
        },
      });
  }

  protected applyFilters(): void {
    this.clearSearchDebounce();
    this.searchPending.set(false);
    this.page.set(1);
    this.loadBalances();
  }

  protected refreshData(): void {
    this.clearSearchDebounce();
    this.searchPending.set(false);
    this.loadFilterOptions(true);
    this.loadBalances(true);
  }

  protected scheduleSearch(search: string): void {
    this.filterModel.search = search;
    this.clearSearchDebounce();
    this.searchPending.set(true);
    this.searchDebounceHandle = setTimeout(() => {
      this.searchPending.set(false);
      this.page.set(1);
      this.loadBalances();
    }, 500);
  }

  protected clearFilters(): void {
    this.clearSearchDebounce();
    this.searchPending.set(false);
    this.filterModel = this.createEmptyFilters();
    this.page.set(1);
    this.pageSize.set(this.filterModel.pageSize);
    this.loadBalances();
  }

  protected changePageSize(pageSize: number): void {
    this.pageSize.set(Number(pageSize));
    this.page.set(1);
    this.loadBalances();
  }

  protected goToPreviousPage(): void {
    if (!this.hasPreviousPage()) {
      return;
    }

    this.page.update((currentPage) => currentPage - 1);
    this.loadBalances();
  }

  protected goToNextPage(): void {
    if (!this.hasNextPage()) {
      return;
    }

    this.page.update((currentPage) => currentPage + 1);
    this.loadBalances();
  }

  private loadFilterOptions(bypassCache = false): void {
    this.bankBalancesApi.getFilterOptions({ bypassCache }).subscribe({
      next: (options) => this.filterOptions.set(options),
      error: (error: unknown) =>
        this.error.set(error instanceof Error ? error.message : 'לא ניתן לטעון את אפשרויות הסינון.'),
    });
  }

  private clearSearchDebounce(): void {
    if (this.searchDebounceHandle === null) {
      return;
    }

    clearTimeout(this.searchDebounceHandle);
    this.searchDebounceHandle = null;
  }

  private restoreStateFromUrl(): void {
    const params = new URLSearchParams(window.location.search);
    this.filterModel.search = params.get('search') ?? '';
    this.filterModel.bankName = params.get('bankName') ?? '';
    this.filterModel.currency = params.get('currency') ?? '';
    this.filterModel.balanceType = params.get('balanceType') ?? '';
    this.filterModel.status = params.get('status') ?? '';
    this.filterModel.minAmount = this.parseNumberParam(params.get('minAmount'));
    this.filterModel.maxAmount = this.parseNumberParam(params.get('maxAmount'));
    this.filterModel.sortBy = params.get('sortBy') ?? 'date';
    this.filterModel.sortDirection = params.get('sortDirection') === 'asc' ? 'asc' : 'desc';

    const page = this.parseNumberParam(params.get('page'));
    const pageSize = this.parseNumberParam(params.get('pageSize'));
    this.page.set(page && page > 0 ? page : 1);
    this.pageSize.set(pageSize && pageSize > 0 ? pageSize : 50);
  }

  private syncUrlState(): void {
    const params = new URLSearchParams();

    this.appendStringParam(params, 'search', this.filterModel.search);
    this.appendStringParam(params, 'bankName', this.filterModel.bankName);
    this.appendStringParam(params, 'currency', this.filterModel.currency);
    this.appendStringParam(params, 'balanceType', this.filterModel.balanceType);
    this.appendStringParam(params, 'status', this.filterModel.status);
    this.appendNumberParam(params, 'minAmount', this.filterModel.minAmount);
    this.appendNumberParam(params, 'maxAmount', this.filterModel.maxAmount);
    this.appendNumberParam(params, 'page', this.page() === 1 ? null : this.page());
    this.appendNumberParam(params, 'pageSize', this.pageSize() === 50 ? null : this.pageSize());
    this.appendStringParam(params, 'sortBy', this.filterModel.sortBy === 'date' ? '' : this.filterModel.sortBy);
    this.appendStringParam(params, 'sortDirection', this.filterModel.sortDirection === 'desc' ? '' : this.filterModel.sortDirection);

    const queryString = params.toString();
    const nextUrl = queryString ? `${window.location.pathname}?${queryString}` : window.location.pathname;
    window.history.replaceState({}, '', nextUrl);
  }

  private parseNumberParam(value: string | null): number | null {
    if (value === null || value.trim() === '') {
      return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private appendStringParam(params: URLSearchParams, key: string, value: string): void {
    const trimmed = value.trim();
    if (trimmed) {
      params.set(key, trimmed);
    }
  }

  private appendNumberParam(params: URLSearchParams, key: string, value: number | null): void {
    if (value !== null && !Number.isNaN(value)) {
      params.set(key, String(value));
    }
  }

  private createEmptyFilters(): BankBalanceFilterForm {
    return {
      search: '',
      bankName: '',
      currency: '',
      balanceType: '',
      status: '',
      minAmount: null,
      maxAmount: null,
      page: 1,
      pageSize: 50,
      sortBy: 'date',
      sortDirection: 'desc',
    };
  }
}
