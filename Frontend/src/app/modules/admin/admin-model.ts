export interface DateRange {
  from: Date;
  to: Date;
}

export interface DashboardLayout {
  widgets: string[];
  charts: string[];
  kpis: string[];
}
