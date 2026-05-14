import {Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
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
  @Input() kpiCards: string[] = [];
  @Output() kpiCardsChange = new EventEmitter<string[]>();

  kpi!: KpiDto;

  constructor(private analyticsService: DashboardAnalyticsService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.range?.from && this.range?.to &&
      (changes['range'] || changes['refreshTick'])) {
      this.load();
    }
  }

  ngOnInit(): void {
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

    this.kpiCardsChange.emit([...this.kpiCards]);
  }


}
