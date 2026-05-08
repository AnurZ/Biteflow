import { Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { Chart } from 'chart.js/auto';
import { ChartService, OrdersPerDayDto } from '../../../services/chart-services';

@Component({
  standalone: false,
  selector: 'app-orders-chart',
  templateUrl: './analytics-dashboard.component.html'
})
export class OrdersChartComponent implements OnChanges, OnDestroy {

  @Input() from!: Date;
  @Input() to!: Date;

  private chart: Chart | null = null;

  constructor(private analyticsService: ChartService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.from && this.to) {
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  load(): void {
    this.analyticsService.getOrdersPerDay(this.from, this.to)
      .subscribe(data => this.render(data));
  }

  render(data: OrdersPerDayDto[]): void {

    const labels = data.map(x => x.date);
    const values = data.map(x => x.count);

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart('ordersChart', {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Orders per day',
          data: values,
          borderWidth: 2
        }]
      }
    });
  }
}
