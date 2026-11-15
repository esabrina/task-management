import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { Task } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';
import { TaskModalComponent } from './task-modal.component';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [DatePipe, TaskModalComponent],
  template: `
    <main class="app-shell">
      <nav class="navbar bg-white border-bottom">
        <div class="container py-1">
          <a class="navbar-brand fw-bold text-primary" href="/tasks">Task Management</a>
          <button type="button" class="btn btn-outline-secondary btn-sm" (click)="logout()">
            Sign out
          </button>
        </div>
      </nav>

      <section class="container py-4 py-md-5">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
          <div>
            <h1 class="h2 mb-1">My tasks</h1>
            <p class="text-secondary mb-0">Keep track of what needs to get done.</p>
          </div>
          <button type="button" class="btn btn-primary" (click)="openCreate()">
            <span aria-hidden="true" class="me-1">+</span> Create Task
          </button>
        </div>

        @if (successMessage()) {
          <div class="alert alert-success" role="status">{{ successMessage() }}</div>
        }
        @if (errorMessage()) {
          <div class="alert alert-danger" role="alert">{{ errorMessage() }}</div>
        }

        <div class="card border-0 shadow-sm">
          @if (isLoading()) {
            <div class="card-body py-5 text-center text-secondary">Loading tasks…</div>
          } @else if (tasks().length === 0) {
            <div class="card-body py-5 text-center">
              <h2 class="h5">No tasks yet</h2>
              <p class="text-secondary mb-3">Create a task to get started.</p>
              <button type="button" class="btn btn-primary" (click)="openCreate()">Create Task</button>
            </div>
          } @else {
            <div class="table-responsive">
              <table class="table align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th scope="col" class="ps-4">Task</th>
                    <th scope="col">Status</th>
                    <th scope="col">Due date</th>
                    <th scope="col" class="text-end pe-4">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  @for (task of tasks(); track task.id) {
                    <tr class="task-row" tabindex="0"
                      (click)="openTask(task.id)"
                      (keydown.enter)="openTask(task.id)"
                      (keydown.space)="openTask(task.id); $event.preventDefault()">
                      <td class="ps-4">
                        <div class="fw-semibold">{{ task.title }}</div>
                        @if (task.description) {
                          <div class="task-description small">{{ task.description }}</div>
                        }
                      </td>
                      <td><span class="badge rounded-pill text-bg-light border">{{ task.status }}</span></td>
                      <td>{{ task.dueDate | date:'mediumDate' }}</td>
                      <td class="text-end pe-4">
                        <button type="button" class="btn btn-sm btn-outline-danger"
                          [attr.aria-label]="'Delete ' + task.title"
                          (click)="deleteTask(task, $event)">Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      </section>

      @if (modalOpen()) {
        <app-task-modal
          [task]="selectedTask()"
          (closed)="closeModal()"
          (saved)="handleSaved($event)" />
      }
    </main>
  `
})
export class TaskListComponent {
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);

  readonly tasks = signal<Task[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly modalOpen = signal(false);
  readonly selectedTask = signal<Task | null>(null);

  constructor() {
    this.loadTasks();
  }

  logout(): void {
    this.authService.logout();
  }

  openCreate(): void {
    this.selectedTask.set(null);
    this.modalOpen.set(true);
  }

  openTask(id: string): void {
    this.errorMessage.set('');
    this.taskService.getTask(id).subscribe({
      next: (task) => {
        this.selectedTask.set(task);
        this.modalOpen.set(true);
      },
      error: () => this.errorMessage.set('Unable to load this task. Please refresh and try again.')
    });
  }

  closeModal(): void {
    this.modalOpen.set(false);
    this.selectedTask.set(null);
  }

  handleSaved(message: string): void {
    this.closeModal();
    this.successMessage.set(message);
    this.loadTasks();
  }

  deleteTask(task: Task, event: MouseEvent): void {
    event.stopPropagation();
    if (!window.confirm(`Delete "${task.title}"? This action cannot be undone.`)) {
      return;
    }

    this.errorMessage.set('');
    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        this.tasks.update((tasks) => tasks.filter((item) => item.id !== task.id));
        this.successMessage.set('Task deleted successfully.');
      },
      error: (error: unknown) => this.errorMessage.set(
        error instanceof HttpErrorResponse && error.status === 404
          ? 'This task was not found or is no longer available.'
          : 'Unable to delete this task. Please try again.'
      )
    });
  }

  private loadTasks(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.taskService.getTasks().subscribe({
      next: (tasks) => {
        this.tasks.set([...tasks].sort(
          (first, second) => Date.parse(first.dueDate) - Date.parse(second.dueDate)
        ));
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load tasks. Please try again.');
        this.isLoading.set(false);
      }
    });
  }
}
