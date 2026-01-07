import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Outstanding } from './outstanding';

describe('Outstanding', () => {
  let component: Outstanding;
  let fixture: ComponentFixture<Outstanding>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Outstanding]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Outstanding);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
