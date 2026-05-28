import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';

import { BankBalanceSummary } from '../../models/bank-balance.model';

@Component({
  selector: 'app-summary-cards',
  imports: [CommonModule],
  templateUrl: './summary-cards.component.html',
})
export class SummaryCardsComponent {
  readonly summary = input<BankBalanceSummary | null>(null);
  readonly currencyTotals = input<readonly [string, number][]>([]);
}
