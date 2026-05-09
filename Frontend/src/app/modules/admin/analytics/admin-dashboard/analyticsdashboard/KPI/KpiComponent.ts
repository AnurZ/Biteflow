import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DashboardAnalyticsService, KpiDto } from '../../../services/DashboardAnalyticsService';
import { DateRange } from '../../../../admin-model';

@Component({
  standalone: false,
  selector: 'app-kpi',
  templateUrl: './kpi.html',
  styleUrls: ['./kpi.css']
})
export class KpiComponent implements OnChanges {

  @Input() range!: DateRange;
  @Input() refreshTick!: number;

  kpi!: KpiDto;

  constructor(private analyticsService: DashboardAnalyticsService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.range?.from && this.range?.to &&
      (changes['range'] || changes['refreshTick'])) {
      this.load();
    }
  }

  load(): void {
    this.analyticsService.getKpis(this.range.from, this.range.to)
      .subscribe({
        next: res => {
          console.log('KPI data', res);
          this.kpi = res;
        },
        error: err => console.error('KPI ERROR', err)
      });
  }
}
