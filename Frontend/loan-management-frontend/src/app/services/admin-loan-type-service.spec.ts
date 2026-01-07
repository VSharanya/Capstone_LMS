import { TestBed } from '@angular/core/testing';

import { AdminLoanTypeService } from './admin-loan-type-service';

describe('AdminLoanTypeService', () => {
  let service: AdminLoanTypeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminLoanTypeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
