import { HttpErrorResponse } from '@angular/common/http';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { UI_CONFIG } from '../../core/config/ui.config';
import { Task } from '../../core/models/task.model';
import { AuthService } from '../../core/services/auth.service';
import { TaskService } from '../../core/services/task.service';
import { TaskListComponent } from './task-list.component';

describe('TaskListComponent', () => {
  let taskService: jasmine.SpyObj<TaskService>;
  let authService: jasmine.SpyObj<AuthService>;
  const firstTask: Task = {
    id: 'later',
    title: 'Later task',
    description: null,
    statusId: 1,
    status: 'To do',
    dueDate: '2030-02-01T00:00:00.000Z',
    dateCreated: '2029-01-01T00:00:00.000Z',
    updatedAt: null
  };
  const secondTask: Task = {
    ...firstTask,
    id: 'sooner',
    title: 'Sooner task',
    dueDate: '2030-01-01T00:00:00.000Z'
  };

  beforeEach(async () => {
    taskService = jasmine.createSpyObj<TaskService>('TaskService', [
      'getTasks', 'getTask', 'deleteTask'
    ]);
    taskService.getTasks.and.returnValue(of([firstTask, secondTask]));
    taskService.getTask.and.returnValue(of(firstTask));
    taskService.deleteTask.and.returnValue(of(void 0));
    authService = jasmine.createSpyObj<AuthService>('AuthService', ['logout']);
    await TestBed.configureTestingModule({
      imports: [TaskListComponent],
      providers: [
        { provide: TaskService, useValue: taskService },
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();
  });

  it('should display the user task list ordered by due date', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.tasks()).toEqual([secondTask, firstTask]);
    expect(component.isLoading()).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('Sooner task');
    expect(fixture.nativeElement.textContent).toContain('Later task');
  });

  it('should sort by status and due date in ascending and descending order', () => {
    const inProgressTask = { ...secondTask, status: 'In progress', statusId: 2 };
    taskService.getTasks.and.returnValue(of([firstTask, inProgressTask]));
    const component = TestBed.createComponent(TaskListComponent).componentInstance;

    expect(component.sortIcon('dueDate')).toBe('↑');
    component.toggleSort('status');
    expect(component.sortedTasks()).toEqual([inProgressTask, firstTask]);
    expect(component.sortIcon('status')).toBe('↑');

    component.toggleSort('status');
    expect(component.sortedTasks()).toEqual([firstTask, inProgressTask]);
    expect(component.sortIcon('status')).toBe('↓');

    component.toggleSort('dueDate');
    expect(component.sortedTasks()).toEqual([inProgressTask, firstTask]);
    expect(component.sortIcon('dueDate')).toBe('↑');

    component.toggleSort('dueDate');
    expect(component.sortedTasks()).toEqual([firstTask, inProgressTask]);
    expect(component.sortIcon('dueDate')).toBe('↓');
  });

  it('should provide a task-creation form without a selected task', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    component.openTask('later');

    component.openCreate();

    expect(component.modalOpen()).toBeTrue();
    expect(component.selectedTask()).toBeNull();
  });

  it('should load a selected task for viewing and show an error when it cannot be loaded', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    component.openTask('later');

    expect(component.modalOpen()).toBeTrue();
    expect(component.selectedTask()).toEqual(firstTask);

    taskService.getTask.and.returnValue(throwError(() => new Error('network')));
    component.openTask('unavailable');
    expect(component.errorMessage()).toBe('Unable to load this task. Please refresh and try again.');
  });

  it('should close the editor and reload the list after a task is saved', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    component.openTask('later');
    taskService.getTasks.calls.reset();
    taskService.getTasks.and.returnValue(of([firstTask]));

    component.handleSaved('Task updated successfully.');

    expect(component.modalOpen()).toBeFalse();
    expect(component.selectedTask()).toBeNull();
    expect(component.successMessage()).toBe('Task updated successfully.');
    expect(taskService.getTasks).toHaveBeenCalled();
    expect(component.tasks()).toEqual([firstTask]);
  });

  it('should clear a success message after the configured duration', fakeAsync(() => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    component.handleSaved('Task updated successfully.');

    tick(UI_CONFIG.successMessageDurationMs - 1);
    expect(component.successMessage()).toBe('Task updated successfully.');

    tick(1);
    expect(component.successMessage()).toBe('');
    fixture.destroy();
  }));

  it('should ask for confirmation and leave the task unchanged when deletion is cancelled', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    const confirm = spyOn(window, 'confirm').and.returnValue(false);
    const event = new MouseEvent('click');
    const stopPropagation = spyOn(event, 'stopPropagation');

    component.deleteTask(firstTask, event);

    expect(confirm).toHaveBeenCalledWith('Delete "Later task"? This action cannot be undone.');
    expect(stopPropagation).toHaveBeenCalled();
    expect(taskService.deleteTask).not.toHaveBeenCalled();
    expect(component.tasks()).toContain(firstTask);
  });

  it('should delete a confirmed task and show a success message', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    spyOn(window, 'confirm').and.returnValue(true);

    component.deleteTask(firstTask, new MouseEvent('click'));

    expect(taskService.deleteTask).toHaveBeenCalledWith('later');
    expect(component.tasks()).toEqual([secondTask]);
    expect(component.successMessage()).toBe('Task deleted successfully.');
  });

  it('should report unavailable tasks and unexpected failures during deletion', () => {
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    spyOn(window, 'confirm').and.returnValue(true);
    taskService.deleteTask.and.returnValue(throwError(
      () => new HttpErrorResponse({ status: 404 })
    ));

    component.deleteTask(firstTask, new MouseEvent('click'));
    expect(component.errorMessage()).toBe('This task was not found or is no longer available.');

    taskService.deleteTask.and.returnValue(throwError(() => new Error('network')));
    component.deleteTask(secondTask, new MouseEvent('click'));
    expect(component.errorMessage()).toBe('Unable to delete this task. Please try again.');
  });

  it('should show a loading failure and allow the user to log out', () => {
    taskService.getTasks.and.returnValue(throwError(() => new Error('network')));
    const fixture = TestBed.createComponent(TaskListComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isLoading()).toBeFalse();
    expect(component.errorMessage()).toBe('Unable to load tasks. Please try again.');
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });
});
