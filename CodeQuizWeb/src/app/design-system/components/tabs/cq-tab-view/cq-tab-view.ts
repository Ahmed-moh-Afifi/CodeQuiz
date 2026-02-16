import {
  Component,
  ContentChildren,
  QueryList,
  AfterContentInit,
  ViewEncapsulation,
} from '@angular/core';
import { CqTab } from '../cq-tab/cq-tab';

@Component({
  selector: 'cq-tab-view',
  standalone: true,
  imports: [],
  templateUrl: './cq-tab-view.html',
  styleUrl: './cq-tab-view.scss',
  encapsulation: ViewEncapsulation.None,
})
export class CqTabView implements AfterContentInit {
  @ContentChildren(CqTab) tabs!: QueryList<CqTab>;

  ngAfterContentInit() {
    // Activate the first tab by default
    if (this.tabs.length > 0 && !this.tabs.find((t) => t.active)) {
      this.selectTab(this.tabs.first);
    }
  }

  selectTab(tab: CqTab) {
    this.tabs.forEach((t) => (t.active = false));
    tab.active = true;
  }
}
