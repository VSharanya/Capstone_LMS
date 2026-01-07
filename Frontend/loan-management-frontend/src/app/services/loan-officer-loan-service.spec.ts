import { TestBed } from '@angular/core/testing';

import { LoanOfficerLoanService } from './loan-officer-loan-service';

describe('LoanOfficerLoanService', () => {
  let service: LoanOfficerLoanService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoanOfficerLoanService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
