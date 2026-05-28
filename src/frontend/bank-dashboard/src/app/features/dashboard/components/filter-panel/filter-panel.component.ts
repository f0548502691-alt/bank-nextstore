import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { BankBalanceFilterOptions } from '../../models/bank-balance.model';
import { BankBalanceFilterForm } from '../../models/bank-balance-filter.model';
import { SortOption } from '../../models/sort-option.model';

@Component({
  selector: 'app-filter-panel',
  imports: [CommonModule, FormsModule],
  templateUrl: './filter-panel.component.html',
})
export class FilterPanelComponent {
  readonly filterModel = input.required<BankBalanceFilterForm>();
  readonly filterOptions = input.required<BankBalanceFilterOptions>();
  readonly sortOptions = input.required<readonly SortOption[]>();
  readonly loading = input(false);
  readonly searchPending = input(false);

  readonly applyFilters = output<void>();
  readonly clearFilters = output<void>();
  readonly searchChange = output<string>();
}
