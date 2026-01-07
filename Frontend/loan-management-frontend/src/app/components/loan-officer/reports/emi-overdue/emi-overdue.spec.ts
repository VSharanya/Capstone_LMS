import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmiOverdue } from './emi-overdue';

describe('EmiOverdue', () => {
  let component: EmiOverdue;
  let fixture: ComponentFixture<EmiOverdue>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmiOverdue]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EmiOverdue);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
