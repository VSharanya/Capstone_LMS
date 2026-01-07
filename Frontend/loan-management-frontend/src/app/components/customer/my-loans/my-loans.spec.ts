import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyLoansComponent } from './my-loans';

describe('MyLoans', () => {
  let component: MyLoansComponent;
  let fixture: ComponentFixture<MyLoansComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyLoansComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyLoansComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
