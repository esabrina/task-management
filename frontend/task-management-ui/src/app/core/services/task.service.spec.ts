import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Task, TaskRequest, TaskStatus } from '../models/task.model';
import { environment } from '../../../environments/environment';
import { TaskService } from './task.service';

describe('TaskService', () => {
  let service: TaskService;
  let httpTestingController: HttpTestingController;
  const baseUrl = `${environment.apiUrl}/api/tasks`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TaskService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(TaskService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTestingController.verify());

  it('should retrieve the authenticated user task list', () => {
    const tasks: Task[] = [];
    service.getTasks().subscribe((result) => expect(result).toEqual(tasks));
    const request = httpTestingController.expectOne(baseUrl);
    expect(request.request.method).toBe('GET');
    request.flush(tasks);
  });

  it('should retrieve task details using an encoded identifier', () => {
    const task: Task = {
      id: 'task/1',
      title: 'Review requirements',
      description: null,
      statusId: 1,
      status: 'To do',
      dueDate: '2030-01-01T00:00:00.000Z',
      dateCreated: '2029-01-01T00:00:00.000Z',
      updatedAt: null
    };
    service.getTask(task.id).subscribe((result) => expect(result).toEqual(task));
    const request = httpTestingController.expectOne(`${baseUrl}/task%2F1`);
    expect(request.request.method).toBe('GET');
    request.flush(task);
  });

  it('should retrieve available task statuses', () => {
    const statuses: TaskStatus[] = [{ id: 1, name: 'To do' }];
    service.getStatuses().subscribe((result) => expect(result).toEqual(statuses));
    const request = httpTestingController.expectOne(`${baseUrl}/status`);
    expect(request.request.method).toBe('GET');
    request.flush(statuses);
  });

  it('should create a task with the submitted details', () => {
    const taskRequest: TaskRequest = {
      title: 'Review requirements',
      description: null,
      statusId: 1,
      dueDate: '2030-01-01T00:00:00.000Z'
    };
    service.createTask(taskRequest).subscribe();
    const request = httpTestingController.expectOne(baseUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(taskRequest);
    request.flush({ id: 'task-1' });
  });

  it('should update a task using an encoded identifier', () => {
    const taskRequest: TaskRequest = {
      title: 'Updated',
      description: 'Details',
      statusId: 2,
      dueDate: '2030-01-02T00:00:00.000Z'
    };
    service.updateTask('task/1', taskRequest).subscribe();
    const request = httpTestingController.expectOne(`${baseUrl}/task%2F1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(taskRequest);
    request.flush({ id: 'task/1' });
  });

  it('should delete a task using an encoded identifier', () => {
    service.deleteTask('task/1').subscribe();
    const request = httpTestingController.expectOne(`${baseUrl}/task%2F1`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
