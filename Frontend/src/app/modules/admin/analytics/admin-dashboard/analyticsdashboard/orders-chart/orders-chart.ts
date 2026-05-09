import {Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges} from '@angular/core';
import { Chart } from 'chart.js/auto';
import { ChartService, OrdersPerDayDto } from '../../../services/chart-services';
import { DashboardRefreshService } from '../../../services/DashboardRefreshService';
import { Subscription } from 'rxjs';
import {DateRange} from '../../../../admin-model';

@Component({
  standalone: false,
  selector: 'app-orders-chart',
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analyticsdashboard.css'],
})
export class OrdersChartComponent implements OnChanges, OnInit, OnDestroy {

  @Input() range!: DateRange;
  @Input() refreshTick!: number;

  private chart: Chart | null = null;
  private sub = new Subscription();

  constructor(
    private analyticsService: ChartService,
    private refresh: DashboardRefreshService
  ) {}

  ngOnInit(): void {


    this.sub.add(
      this.refresh.refresh$.subscribe(() => {
        this.load();
      })
    );

    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.range?.from && this.range?.to &&
      (changes['range'] || changes['refreshTick'])) {
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.sub.unsubscribe();
  }

  load(): void {

    if (!this.range?.from || !this.range?.to) return;

    this.analyticsService.getOrdersPerDay(this.range)
      .subscribe(data => {
        this.render(data);
        console.log('OrdersChartComponent data', data);
      });
  }

  render(data: OrdersPerDayDto[]): void {

    const labels = data.map(x => x.date);
    const values = data.map(x => x.count);

    this.chart?.destroy();

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
