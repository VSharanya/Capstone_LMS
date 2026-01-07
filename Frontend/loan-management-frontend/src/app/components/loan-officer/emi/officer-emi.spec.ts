import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OfficerEmiComponent } from './officer-emi';

describe('OfficerEmiComponent', () => {
  let component: OfficerEmiComponent;
  let fixture: ComponentFixture<OfficerEmiComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OfficerEmiComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OfficerEmiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
