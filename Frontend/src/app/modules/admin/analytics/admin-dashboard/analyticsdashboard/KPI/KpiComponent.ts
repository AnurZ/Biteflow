import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import { DashboardAnalyticsService, KpiDto } from '../../../services/DashboardAnalyticsService';
import { DateRange } from '../../../../admin-model';
import {CdkDragDrop, moveItemInArray} from '@angular/cdk/drag-drop';


@Component({
  standalone: false,
  selector: 'app-kpi',
  templateUrl: './kpi.html',
  styleUrls: ['./kpi.css']
})
export class KpiComponent implements OnChanges, OnInit {

  @Input() range!: DateRange;
  @Input() refreshTick!: number;
  @Input() isEditMode = false;

  kpi!: KpiDto;

  kpiCards = [
    'orders',
    'revenue',
    'avgOrder',
    'topItem'
  ];

  constructor(private analyticsService: DashboardAnalyticsService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.range?.from && this.range?.to &&
      (changes['range'] || changes['refreshTick'])) {
      this.load();
    }
  }

  ngOnInit(): void {
    this.loadLayout();
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

  dropKpi(event: CdkDragDrop<string[]>): void {

    moveItemInArray(
      this.kpiCards,
      event.previousIndex,
      event.currentIndex
    );

    this.saveLayout();
  }

  private saveLayout(): void {

    localStorage.setItem(
      'admin-kpi-layout',
      JSON.stringify(this.kpiCards)
    );
  }

  private loadLayout(): void {

    const layout = localStorage.getItem(
      'admin-kpi-layout'
    );

    if (!layout) return;

    this.kpiCards = JSON.parse(layout);
  }
}
