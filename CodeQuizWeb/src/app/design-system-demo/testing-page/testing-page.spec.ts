import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestingPage } from './testing-page';

describe('TestingPage', () => {
  let component: TestingPage;
  let fixture: ComponentFixture<TestingPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestingPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TestingPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
