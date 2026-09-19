
import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges
} from '@angular/core';
import { Chart } from 'chart.js/auto';
import {
  ChartService,
  RevenuePerDayDto
} from '../../../services/chart-services';
import { DashboardRefreshService } from '../../../services/DashboardRefreshService';
import { Subscription } from 'rxjs';
import { DateRange } from '../../../../admin-model';
import { CurrencyService} from '../../../../../../services/currency-service/currency.service';

@Component({
  standalone: false,
  selector: 'app-revenue-chart',
  templateUrl: './revenue-chart.html',
  styleUrls: ['./revenue-chart.css'],
})
export class RevenueChart implements OnChanges, OnInit, OnDestroy {

  @Input() range!: DateRange;
  @Input() refreshTick!: number;

  chart: any;
  private sub = new Subscription();

  constructor(
    private service: ChartService,
    private refresh: DashboardRefreshService,
    private currencyService: CurrencyService
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
    if (
      this.range?.from &&
      this.range?.to &&
      (changes['range'] || changes['refreshTick'])
    ) {
      this.load();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
    this.sub.unsubscribe();
  }

  load(): void {

    if (!this.range?.from || !this.range?.to) return;

    this.service.getRevenuePerDay(this.range)
      .subscribe(data => {
        this.render(data);
      });
  }

  render(data: RevenuePerDayDto[]): void {

    this.chart?.destroy();

    this.chart = new Chart('revenueChart', {
      type: 'line',

      data: {
        labels: data.map(x => x.date),

        datasets: [{
          label: 'Revenue',
          data: data.map(x => x.revenue),
          borderWidth: 2
        }]
      },

      options: {
        plugins: {
          tooltip: {
            callbacks: {
              label: (context) => {
                return this.currencyService.format(context.parsed.y);
              }
            }
          }
        }
      }
    });
  }
}
