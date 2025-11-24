import { Component } from '@angular/core';

interface Layout {
  id: number;
  title: string;
  tableCount: number[];
}

@Component({
  selector: 'app-table-layout',
  standalone: false,
  templateUrl: './table-layout.html',
  styleUrls: ['./table-layout.css']
})
export class TableLayoutComponent {

  layouts: Layout[] = [
    { id: 1, title: 'Layout 1', tableCount: [1,1,1,1,1] },
    { id: 2, title: 'Layout 2', tableCount: [1,1,1,1] },
    { id: 3, title: 'Layout 3', tableCount: [1,1,1,1,1,1] },
    { id: 4, title: 'Layout 4', tableCount: [1,1,1,1,1,1,1] },
  ];

  selectedLayout: Layout | null = null;
  isAnimating = false;
  selectLayout(layout: Layout) {
    this.isAnimating = true;
    setTimeout(() => {
      this.selectedLayout = layout;
      this.isAnimating = false;
    }, 150); // small delay for animation
  }

  get otherLayouts(): Layout[] {
    return this.layouts.filter(l => l !== this.selectedLayout);
  }
}
