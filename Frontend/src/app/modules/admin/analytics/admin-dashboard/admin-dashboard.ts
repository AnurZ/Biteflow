import { Component, OnInit } from '@angular/core';
import { DashboardAnalyticsService, KpiDto } from './DashboardAnalyticsService';

@Component({
  standalone: false,
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {

  kpi!: KpiDto;

  from!: Date;
  to!: Date;

  fromDate!: string;
  toDate!: string;

  constructor(private analyticsService: DashboardAnalyticsService) {}

  ngOnInit(): void {
    const today = new Date();
    const weekAgo = new Date();
    weekAgo.setDate(today.getDate() - 7);

    this.from = weekAgo;
    this.to = today;

    this.fromDate = this.toInput(weekAgo);
    this.toDate = this.toInput(today);

    this.loadAll();
  }

  applyFilter() {
    this.from = new Date(this.fromDate);
    this.to = new Date(this.toDate);
    this.loadAll();
  }

  loadAll() {
    this.analyticsService.getKpis(this.from, this.to)
      .subscribe(res => this.kpi = res);
  }

  private toInput(date: Date): string {
    return date.toISOString().split('T')[0];
  }
}
