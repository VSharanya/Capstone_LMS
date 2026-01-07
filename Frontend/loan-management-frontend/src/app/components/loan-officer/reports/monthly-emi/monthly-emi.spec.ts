import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MonthlyEmi } from './monthly-emi';

describe('MonthlyEmi', () => {
  let component: MonthlyEmi;
  let fixture: ComponentFixture<MonthlyEmi>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthlyEmi]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MonthlyEmi);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
