import { Component, computed, inject, OnDestroy, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { UI_CONFIG } from '../../core/config/ui.config';
import { AuthService } from '../../core/services/auth.service';
import { Task } from '../../core/models/task.model';
import { TaskService } from '../../core/services/task.service';
import { TaskModalComponent } from './task-modal.component';

type SortField = 'status' | 'dueDate';
type SortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [DatePipe, TaskModalComponent, NgbTooltipModule],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnDestroy {
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);

  readonly tasks = signal<Task[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly modalOpen = signal(false);
  readonly selectedTask = signal<Task | null>(null);
  readonly sortField = signal<SortField>('dueDate');
  readonly sortDirection = signal<SortDirection>('asc');
  readonly sortedTasks = computed(() => {
    const field = this.sortField();
    const direction = this.sortDirection() === 'asc' ? 1 : -1;

    return [...this.tasks()].sort((first, second) => {
      const comparison = field === 'status'
        ? first.status.localeCompare(second.status, undefined, { sensitivity: 'base' })
        : Date.parse(first.dueDate) - Date.parse(second.dueDate);

      return comparison * direction;
    });
  });

  private successMessageTimer?: ReturnType<typeof setTimeout>;

  constructor() {
    this.loadTasks();
  }

  ngOnDestroy(): void {
    this.clearSuccessMessageTimer();
  }

  logout(): void {
    this.authService.logout();
  }

  toggleSort(field: SortField): void {
    if (this.sortField() === field) {
      this.sortDirection.update((direction) => direction === 'asc' ? 'desc' : 'asc');
      return;
    }

    this.sortField.set(field);
    this.sortDirection.set('asc');
  }

  sortIcon(field: SortField): string {
    if (this.sortField() !== field) {
      return '↕';
    }

    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }

  sortActionLabel(field: SortField): string {
    const direction = this.sortField() === field && this.sortDirection() === 'asc'
      ? 'descending'
      : 'ascending';
    const label = field === 'status' ? 'status' : 'due date';

    return `Sort by ${label} ${direction}`;
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
    this.showSuccessMessage(message);
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
        this.showSuccessMessage('Task deleted successfully.');
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

  private showSuccessMessage(message: string): void {
    this.clearSuccessMessageTimer();
    this.successMessage.set(message);
    this.successMessageTimer = setTimeout(() => {
      this.successMessage.set('');
      this.successMessageTimer = undefined;
    }, UI_CONFIG.successMessageDurationMs);
  }

  private clearSuccessMessageTimer(): void {
    if (this.successMessageTimer !== undefined) {
      clearTimeout(this.successMessageTimer);
      this.successMessageTimer = undefined;
    }
  }
}
