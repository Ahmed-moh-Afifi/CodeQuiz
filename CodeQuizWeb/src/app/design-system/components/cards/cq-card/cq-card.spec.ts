import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CqCard } from './cq-card';

describe('CqCard', () => {
  let component: CqCard;
  let fixture: ComponentFixture<CqCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CqCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CqCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
