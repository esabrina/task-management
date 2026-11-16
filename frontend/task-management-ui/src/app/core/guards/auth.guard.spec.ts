import { TestBed } from '@angular/core/testing';
import { Router, UrlSegmentGroup, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let isAuthenticated: jasmine.Spy;
  let clearExpiredSession: jasmine.Spy;
  let createUrlTree: jasmine.Spy;
  let loginUrl: UrlTree;

  beforeEach(() => {
    isAuthenticated = jasmine.createSpy('isAuthenticated');
    clearExpiredSession = jasmine.createSpy('clearExpiredSession');
    createUrlTree = jasmine.createSpy('createUrlTree');
    loginUrl = new UrlTree(new UrlSegmentGroup([], {}), {});
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isAuthenticated, clearExpiredSession } },
        { provide: Router, useValue: { createUrlTree } }
      ]
    });
    createUrlTree.and.returnValue(loginUrl);
  });

  const activate = () => TestBed.runInInjectionContext(() => authGuard(null!, null!));

  it('should allow access to the task area when the user is authenticated', () => {
    isAuthenticated.and.returnValue(true);

    expect(activate()).toBeTrue();
    expect(clearExpiredSession).not.toHaveBeenCalled();
    expect(createUrlTree).not.toHaveBeenCalled();
  });

  it('should clear an expired session and redirect unauthenticated users to login', () => {
    isAuthenticated.and.returnValue(false);

    expect(activate()).toBe(loginUrl);
    expect(clearExpiredSession).toHaveBeenCalled();
    expect(createUrlTree).toHaveBeenCalledWith(['/login']);
  });
});
