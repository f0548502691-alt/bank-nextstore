import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  BankBalance,
  BankBalanceFilterOptions,
  BankBalanceSummary,
} from './features/dashboard/models/bank-balance.model';
import { BankBalanceFilterForm } from './features/dashboard/models/bank-balance-filter.model';
import { BankBalancesApiService } from './features/dashboard/services/bank-balances-api.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly bankBalancesApi = inject(BankBalancesApiService);

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
  protected readonly page = signal(1);
  protected readonly pageSize = signal(50);
  protected readonly totalPages = signal(0);
  protected readonly totalCount = signal(0);
  protected readonly hasPreviousPage = signal(false);
  protected readonly hasNextPage = signal(false);
  protected readonly pageSizeOptions = [25, 50, 100, 250, 500];
  protected readonly sortOptions = [
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
    this.loadFilterOptions();
    this.loadBalances();
  }

  protected loadBalances(): void {
    this.loading.set(true);
    this.error.set(null);
    this.filterModel.page = this.page();
    this.filterModel.pageSize = this.pageSize();

    this.bankBalancesApi
      .getBalances(this.filterModel)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.balances.set(response.items);
          this.summary.set(response.summary);
          this.page.set(response.page);
          this.pageSize.set(response.pageSize);
          this.totalPages.set(response.totalPages);
          this.totalCount.set(response.totalCount);
          this.hasPreviousPage.set(response.hasPreviousPage);
          this.hasNextPage.set(response.hasNextPage);
        },
        error: (error: unknown) => {
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
    this.page.set(1);
    this.loadBalances();
  }

  protected clearFilters(): void {
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

  private loadFilterOptions(): void {
    this.bankBalancesApi.getFilterOptions().subscribe({
      next: (options) => this.filterOptions.set(options),
      error: (error: unknown) =>
        this.error.set(error instanceof Error ? error.message : 'לא ניתן לטעון את אפשרויות הסינון.'),
    });
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
