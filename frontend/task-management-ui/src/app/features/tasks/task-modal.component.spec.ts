import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Task, TaskStatus } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';
import { TaskModalComponent } from './task-modal.component';

describe('TaskModalComponent', () => {
  let taskService: jasmine.SpyObj<TaskService>;
  const statuses: TaskStatus[] = [{ id: 1, name: 'To do' }, { id: 2, name: 'Done' }];
  const savedTask: Task = {
    id: 'task-1',
    title: 'Release plan',
    description: null,
    statusId: 1,
    status: 'To do',
    dueDate: '2030-01-02T00:00:00.000Z',
    dateCreated: '2029-01-01T00:00:00.000Z',
    updatedAt: null
  };

  beforeEach(async () => {
    taskService = jasmine.createSpyObj<TaskService>('TaskService', [
      'getStatuses', 'createTask', 'updateTask'
    ]);
    taskService.getStatuses.and.returnValue(of(statuses));
    taskService.createTask.and.returnValue(of(savedTask));
    taskService.updateTask.and.returnValue(of(savedTask));
    await TestBed.configureTestingModule({
      imports: [TaskModalComponent],
      providers: [{ provide: TaskService, useValue: taskService }]
    }).compileComponents();
  });

  it('should load task statuses and show a useful message when none are available', () => {
    taskService.getStatuses.and.returnValue(of([]));
    const fixture = TestBed.createComponent(TaskModalComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.statuses).toEqual([]);
    expect(fixture.componentInstance.statusError)
      .toBe('No task statuses are available. Please try again later.');
  });

  it('should show a recoverable error when statuses cannot be loaded', () => {
    taskService.getStatuses.and.returnValue(throwError(() => new Error('network')));
    const fixture = TestBed.createComponent(TaskModalComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.statusError)
      .toBe('Unable to load task statuses. Please close and try again.');
    fixture.componentInstance.submit();
    expect(taskService.createTask).not.toHaveBeenCalled();
  });

  it('should create a task with trimmed details and a normalized due date', () => {
    const fixture = TestBed.createComponent(TaskModalComponent);
    const component = fixture.componentInstance;
    const saved = jasmine.createSpy('saved');
    component.saved.subscribe(saved);
    fixture.detectChanges();
    component.form.setValue({
      title: '  Plan release  ',
      description: '  Review scope  ',
      statusId: 1,
      dueDate: '2030-01-02'
    });

    component.submit();

    expect(taskService.createTask).toHaveBeenCalledWith({
      title: 'Plan release',
      description: 'Review scope',
      statusId: 1,
      dueDate: '2030-01-02T00:00:00.000Z'
    });
    expect(saved).toHaveBeenCalledWith('Task created successfully.');
    expect(component.isSaving).toBeFalse();
  });

  it('should reject missing required fields, blank titles, and impossible dates', () => {
    const fixture = TestBed.createComponent(TaskModalComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    component.submit();

    expect(taskService.createTask).not.toHaveBeenCalled();
    expect(component.showError('title')).toBeTrue();
    expect(component.showError('statusId')).toBeTrue();
    expect(component.showError('dueDate')).toBeTrue();

    component.form.setValue({
      title: '   ',
      description: '',
      statusId: 1,
      dueDate: '2030-02-28'
    });
    component.submit();

    expect(taskService.createTask).not.toHaveBeenCalled();
    expect(component.form.controls.title.hasError('required')).toBeTrue();

    component.form.setValue({
      title: 'Valid title',
      description: '',
      statusId: 1,
      dueDate: '2030-02-31'
    });
    component.submit();
    expect(component.form.controls.dueDate.hasError('date')).toBeTrue();
  });

  it('should load task details and persist valid edits', () => {
    const task: Task = {
      id: 'task-1',
      title: 'Original title',
      description: 'Original description',
      statusId: 1,
      status: 'To do',
      dueDate: '2030-01-01T00:00:00.000Z',
      dateCreated: '2029-01-01T00:00:00.000Z',
      updatedAt: null
    };
    const fixture = TestBed.createComponent(TaskModalComponent);
    fixture.componentInstance.task = task;
    fixture.detectChanges();
    const saved = jasmine.createSpy('saved');
    fixture.componentInstance.saved.subscribe(saved);

    expect(fixture.componentInstance.form.getRawValue()).toEqual({
      title: 'Original title',
      description: 'Original description',
      statusId: 1,
      dueDate: '2030-01-01'
    });
    fixture.componentInstance.form.controls.title.setValue('Updated title');
    fixture.componentInstance.submit();

    expect(taskService.updateTask).toHaveBeenCalledWith('task-1', jasmine.objectContaining({
      title: 'Updated title'
    }));
    expect(saved).toHaveBeenCalledWith('Task updated successfully.');
  });

  it('should emit close requests and report an unavailable task when an edit returns 404', () => {
    taskService.updateTask.and.returnValue(throwError(
      () => new HttpErrorResponse({ status: 404 })
    ));
    const fixture = TestBed.createComponent(TaskModalComponent);
    const component = fixture.componentInstance;
    const closed = jasmine.createSpy('closed');
    component.closed.subscribe(closed);
    component.task = {
      id: 'missing',
      title: 'Task',
      description: null,
      statusId: 1,
      status: 'To do',
      dueDate: '2030-01-01T00:00:00.000Z',
      dateCreated: '2029-01-01T00:00:00.000Z',
      updatedAt: null
    };
    fixture.detectChanges();
    component.form.controls.title.setValue('Updated');

    component.close();
    component.submit();

    expect(closed).toHaveBeenCalled();
    expect(component.errorMessage).toBe('This task was not found or is no longer available.');
    expect(component.isSaving).toBeFalse();
  });
});
