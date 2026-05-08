import { Component, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { Chart } from 'chart.js/auto';
import { ChartService, TopSellingItemDto } from '../../../services/chart-services';

@Component({
  standalone: false,
  selector: 'app-top-selling-chart',
  templateUrl: './top-selling-chart.html'
})
export class TopSellingChart implements OnChanges, OnDestroy {

  @Input() from!: Date;
  @Input() to!: Date;

  private chart: Chart | null = null;

  constructor(private analyticsService: ChartService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.from && this.to) {
      this.loadData();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.chart = null;
  }

  loadData(): void {
    this.analyticsService.getTopSellingItems(this.from, this.to)
      .subscribe(data => this.renderChart(data));
  }

  renderChart(data: TopSellingItemDto[]): void {

    const labels = data.map(x => x.itemName);
    const values = data.map(x => x.quantity);

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart('topSellingChart', {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Top Selling Items',
          data: values,
          borderWidth: 2
        }]
      }
    });
  }
}
