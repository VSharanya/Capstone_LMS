import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanTypes } from './loan-types';

describe('LoanTypes', () => {
  let component: LoanTypes;
  let fixture: ComponentFixture<LoanTypes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanTypes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanTypes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
