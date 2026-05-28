import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { BankBalance } from '../../models/bank-balance.model';

@Component({
  selector: 'app-balance-table',
  imports: [CommonModule, FormsModule],
  templateUrl: './balance-table.component.html',
})
export class BalanceTableComponent {
  readonly balances = input<BankBalance[]>([]);
  readonly loading = input(false);
  readonly totalCount = input(0);
  readonly page = input(1);
  readonly totalPages = input(0);
  readonly pageSize = input(50);
  readonly pageSizeOptions = input<readonly number[]>([]);
  readonly hasPreviousPage = input(false);
  readonly hasNextPage = input(false);

  readonly previousPage = output<void>();
  readonly nextPage = output<void>();
  readonly pageSizeChange = output<number>();
}
