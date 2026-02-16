import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CqButton } from './cq-button';

describe('CqButton', () => {
  let component: CqButton;
  let fixture: ComponentFixture<CqButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CqButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CqButton);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
