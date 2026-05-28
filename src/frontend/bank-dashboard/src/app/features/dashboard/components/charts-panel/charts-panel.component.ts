import { CommonModule } from '@angular/common';
import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-charts-panel',
  imports: [CommonModule],
  templateUrl: './charts-panel.component.html',
})
export class ChartsPanelComponent {
  readonly currencyTotals = input<readonly [string, number][]>([]);

  protected readonly totalAmount = computed(() =>
    this.currencyTotals().reduce((sum, [, amount]) => sum + Math.abs(amount), 0)
  );

  protected readonly maxAmount = computed(() =>
    Math.max(...this.currencyTotals().map(([, amount]) => Math.abs(amount)), 0)
  );

  protected readonly donutGradient = computed(() => {
    if (this.currencyTotals().length === 0 || this.totalAmount() === 0) {
      return '#e2e8f0';
    }

    const colors = ['#2f5bd3', '#16a34a', '#f59e0b', '#9333ea', '#ef4444'];
    let start = 0;

    return this.currencyTotals()
      .map(([, amount], index) => {
        const end = start + (Math.abs(amount) / this.totalAmount()) * 100;
        const color = colors[index % colors.length];
        const segment = `${color} ${start}% ${end}%`;
        start = end;
        return segment;
      })
      .join(', ');
  });

  protected percentage(amount: number): number {
    const maxAmount = this.maxAmount();
    return maxAmount === 0 ? 0 : Math.round((Math.abs(amount) / maxAmount) * 100);
  }

  protected share(amount: number): number {
    const totalAmount = this.totalAmount();
    return totalAmount === 0 ? 0 : Math.round((Math.abs(amount) / totalAmount) * 100);
  }

}
