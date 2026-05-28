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

    this.bankBalancesApi
      .getBalances(this.filterModel)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.balances.set(response.items);
          this.summary.set(response.summary);
        },
        error: () => {
          this.balances.set([]);
          this.summary.set(null);
          this.error.set('לא ניתן לטעון את נתוני היתרות כרגע. נסו שוב מאוחר יותר.');
        },
      });
  }

  protected clearFilters(): void {
    this.filterModel = this.createEmptyFilters();
    this.loadBalances();
  }

  private loadFilterOptions(): void {
    this.bankBalancesApi.getFilterOptions().subscribe({
      next: (options) => this.filterOptions.set(options),
      error: () => this.error.set('לא ניתן לטעון את אפשרויות הסינון.'),
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
    };
  }
}
