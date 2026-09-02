import { Component, ElementRef, OnDestroy, PLATFORM_ID, effect, inject, input, viewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Chart, type ChartConfiguration, registerables } from 'chart.js';

import { ThemeService } from '@/services/theme.service';

Chart.register(...registerables);

export interface TrendPoint {
  label: string;
  value: number;
  targetValue: number;
  title: string;
}

const GREEN = '#00a63e';
const BLUE = '#155dfc';

@Component({
  selector: 'app-trend-chart',
  imports: [],
  templateUrl: './trend-chart.html',
})
export class TrendChart implements OnDestroy {
  private readonly themeService = inject(ThemeService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart: Chart | null = null;

  readonly data = input.required<TrendPoint[]>();
  readonly emptyMessage = input('Keine Daten.');
  readonly valueFormatter = input<(value: number) => string>(value => value.toFixed(1));

  constructor() {
    effect(() => {
      const points = this.data();
      const dark = this.themeService.isDark();
      const formatter = this.valueFormatter();
      if (this.isBrowser) {
        this.render(points, dark, formatter);
      }
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private render(points: TrendPoint[], dark: boolean, formatter: (value: number) => string): void {
    const canvas = this.canvasRef()?.nativeElement;
    if (!canvas || points.length === 0) {
      this.chart?.destroy();
      this.chart = null;
      return;
    }

    const textColor = dark ? '#a1a1aa' : '#71717a';
    const gridColor = dark ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.08)';

    const config: ChartConfiguration<'line'> = {
      type: 'line',
      data: {
        labels: points.map(p => p.label),
        datasets: [
          {
            label: 'Ist',
            data: points.map(p => p.value),
            borderColor: GREEN,
            backgroundColor: dark ? 'rgba(0,166,62,0.15)' : 'rgba(0,166,62,0.1)',
            fill: true,
            tension: 0.25,
            pointRadius: 0,
            pointHoverRadius: 4,
            pointBackgroundColor: GREEN,
            borderWidth: 2,
          },
          {
            label: 'Soll',
            data: points.map(p => p.targetValue),
            borderColor: BLUE,
            backgroundColor: 'transparent',
            fill: false,
            tension: 0.25,
            pointRadius: 0,
            pointHoverRadius: 4,
            pointBackgroundColor: BLUE,
            borderWidth: 2,
            borderDash: [6, 4],
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: {
            display: true,
            position: 'top',
            align: 'end',
            labels: { color: textColor, boxWidth: 12, boxHeight: 2, usePointStyle: false },
          },
          tooltip: {
            callbacks: {
              label: ctx => `${ctx.dataset.label}: ${formatter(ctx.parsed.y ?? 0)}`,
            },
          },
        },
        scales: {
          x: {
            ticks: { color: textColor, autoSkip: true, maxRotation: 0 },
            grid: { display: false },
          },
          y: {
            beginAtZero: true,
            ticks: {
              color: textColor,
              callback: value => formatter(Number(value)),
            },
            grid: { color: gridColor },
          },
        },
      },
    };

    if (this.chart) {
      this.chart.data = config.data;
      this.chart.options = config.options ?? {};
      this.chart.update();
    } else {
      this.chart = new Chart(canvas, config);
    }
  }
}
