import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CqPage } from './cq-page';

describe('CqPage', () => {
  let component: CqPage;
  let fixture: ComponentFixture<CqPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CqPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CqPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
