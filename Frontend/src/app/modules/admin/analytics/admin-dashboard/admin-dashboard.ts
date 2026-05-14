import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import { Subscription } from 'rxjs';

import {
  CdkDragDrop,
  moveItemInArray
} from '@angular/cdk/drag-drop';

import { DashboardAnalyticsService } from '../services/DashboardAnalyticsService';
import { RealtimeHubService } from '../../../../services/realtime/realtime-hub.service';
import { DashboardRefreshService } from '../services/DashboardRefreshService';

import {DashboardLayout, DateRange} from '../../admin-model';
import {DashboardLayoutService} from '../../../../services/DashboardLayoutService/DashboardLayout-service';

@Component({
  standalone: false,
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard implements OnInit, OnDestroy {

  private sub = new Subscription();

  refreshTick = 0;

  range!: DateRange;

  fromDate!: string;
  toDate!: string;

  isTodayMode = false;

  isEditMode = false;

  // ================================
  // DASHBOARD LAYOUT STATE
  // ================================

  widgets = [
    'liveOrders',
    'charts'
  ];

  charts = [
    'revenue',
    'orders',
    'topSelling'
  ];

  kpiCards = [
    'orders',
    'revenue',
    'avgOrder',
    'topItem'
  ];

  constructor(
    private realtime: RealtimeHubService,
    private dashboardLayoutService: DashboardLayoutService,
  ) {}

  // ================================
  // INIT
  // ================================
  ngOnInit(): void {

    this.initDates();

    this.loadLayout();

    this.logRange('INIT');

    this.sub.add(
      this.realtime.on('DashboardUpdated').subscribe(() => {

        console.log('📡 DASHBOARD EVENT RECEIVED');

        this.refreshTick++;
      })
    );
  }

  // ================================
  // DRAG DROP
  // ================================
  dropWidgets(event: CdkDragDrop<string[]>): void {

    moveItemInArray(
      this.widgets,
      event.previousIndex,
      event.currentIndex
    );

    this.saveLayout();
  }

  dropCharts(event: CdkDragDrop<string[]>): void {

    moveItemInArray(
      this.charts,
      event.previousIndex,
      event.currentIndex
    );

    this.saveLayout();
  }

  dropKpi(event: CdkDragDrop<string[]>): void {

    moveItemInArray(
      this.kpiCards,
      event.previousIndex,
      event.currentIndex
    );

  }

  // ================================
  // SAVE / LOAD LAYOUT
  // ================================

  toggleEditMode(): void {

    if (this.isEditMode) {

      this.saveLayout();
      this.isEditMode = false;

      return;
    }

    this.isEditMode = true;
  }
  private saveLayout(): void {

    const layout: DashboardLayout = {
      widgets: this.widgets,
      charts: this.charts,
      kpis: this.kpiCards
    };

    this.dashboardLayoutService.saveLayout(
      JSON.stringify(layout)
    ).subscribe({
      next: () => {
        console.log('LAYOUT SAVED');
      },
      error: err => {
        console.error('SAVE ERROR', err);
      }
    });
  }

  private loadLayout(): void {

    this.dashboardLayoutService.getLayout().subscribe({
      next: res => {

        if (!res.layoutJson) return;

        const layout: DashboardLayout = JSON.parse(res.layoutJson);

        if (layout.widgets) this.widgets = layout.widgets;
        if (layout.charts) this.charts = layout.charts;
        if (layout.kpis) this.kpiCards = layout.kpis;

      },
      error: err => {
        console.error('LOAD LAYOUT ERROR', err);
      }
    });
  }

  // ================================
  // TODAY TOGGLE
  // ================================
  toggleTodayMode(): void {

    if (this.isTodayMode) {

      this.setLast7Days();

      this.logRange('LAST 7 DAYS');

      return;
    }

    this.setToday();

    this.logRange('TODAY MODE');
  }

  // ================================
  // TODAY
  // ================================
  private setToday(): void {

    const today = new Date();

    this.range = {
      from: this.startOfDay(today),
      to: this.startOfNextDay(today)
    };

    this.syncInputs();

    this.isTodayMode = true;

    this.refreshTick++;
  }

  // ================================
  // LAST 7 DAYS
  // ================================
  private setLast7Days(): void {

    const today = new Date();

    const from = new Date(today);

    from.setDate(today.getDate() - 7);

    this.range = {
      from: this.startOfDay(from),
      to: this.startOfNextDay(today)
    };

    this.syncInputs();

    this.isTodayMode = false;

    this.refreshTick++;
  }

  // ================================
  // APPLY FILTER
  // ================================
  applyFilter(): void {

    this.isTodayMode = false;

    const from = this.startOfDay(
      new Date(this.fromDate)
    );

    const to = this.startOfNextDay(
      new Date(this.toDate)
    );

    this.range = { from, to };

    this.logRange('APPLY FILTER');

    this.refreshTick++;
  }

  // ================================
  // HELPERS
  // ================================
  private startOfDay(date: Date): Date {

    return new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      0, 0, 0, 0
    );
  }

  private startOfNextDay(date: Date): Date {

    return new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate() + 1,
      0, 0, 0, 0
    );
  }

  private toInput(date: Date): string {

    const year = date.getFullYear();

    const month = String(
      date.getMonth() + 1
    ).padStart(2, '0');

    const day = String(
      date.getDate()
    ).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private syncInputs(): void {

    this.fromDate = this.toInput(this.range.from);

    this.toDate = this.toInput(this.range.to);
  }

  // ================================
  // RANGE LOGGER
  // ================================
  private logRange(context: string): void {

    console.log(`========== ${context} ==========`);

    console.log('FROM LOCAL:', this.range.from);

    console.log('TO LOCAL  :', this.range.to);

    console.log(
      'FROM ISO  :',
      this.range.from.toISOString()
    );

    console.log(
      'TO ISO    :',
      this.range.to.toISOString()
    );

    console.log('================================');
  }

  // ================================
  // INIT DEFAULT
  // ================================
  private initDates(): void {

    this.setLast7Days();
  }

  // ================================
  // CLEANUP
  // ================================
  ngOnDestroy(): void {

    this.sub.unsubscribe();
  }
}
