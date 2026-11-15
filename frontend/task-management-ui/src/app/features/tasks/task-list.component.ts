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
  templateUrl: './task-list.component.html'
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
