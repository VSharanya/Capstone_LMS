import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanTypeDialogComponent } from './loan-type-dialog';

describe('LoanTypeDialog', () => {
  let component: LoanTypeDialogComponent;
  let fixture: ComponentFixture<LoanTypeDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanTypeDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoanTypeDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
