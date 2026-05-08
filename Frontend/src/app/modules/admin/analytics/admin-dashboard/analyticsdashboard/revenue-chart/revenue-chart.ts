import { Component, Input, OnChanges, OnDestroy } from '@angular/core';
import { Chart } from 'chart.js/auto';
import { ChartService, RevenuePerDayDto } from '../../../services/chart-services';

@Component({
  standalone: false,
  selector: 'app-revenue-chart',
  templateUrl: './revenue-chart.html'
})
export class RevenueChart implements OnChanges, OnDestroy {

  @Input() from!: Date;
  @Input() to!: Date;

  chart: any;

  constructor(private service: ChartService) {}

  ngOnChanges(): void {
    if (this.from && this.to) {
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  load() {
    this.service.getRevenuePerDay(this.from, this.to)
      .subscribe(data => this.render(data));
  }

  render(data: RevenuePerDayDto[]) {

    if (this.chart) this.chart.destroy();

    this.chart = new Chart('revenueChart', {
      type: 'line',
      data: {
        labels: data.map(x => x.date),
        datasets: [{
          label: 'Revenue',
          data: data.map(x => x.revenue),
          borderWidth: 2
        }]
      }
    });
  }
}
