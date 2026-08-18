import { Component, computed, input } from '@angular/core';

export interface BarChartDatum {
  label: string;
  value: number;
  displayValue: string;
}

@Component({
  selector: 'app-bar-chart',
  imports: [],
  templateUrl: './bar-chart.html',
})
export class BarChart {
  readonly data = input.required<BarChartDatum[]>();
  readonly emptyMessage = input('Keine Daten.');

  protected readonly maxValue = computed(() => Math.max(1, ...this.data().map(d => d.value)));

  protected widthPct(value: number): number {
    return Math.max(0, (value / this.maxValue()) * 100);
  }

  protected barTitle(item: BarChartDatum): string {
    return `${item.label}: ${item.displayValue}`;
  }
}
