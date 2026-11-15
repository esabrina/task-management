export interface Task {
  id: string;
  title: string;
  description: string | null;
  statusId: number;
  status: string;
  dueDate: string;
  dateCreated: string;
  updatedAt: string | null;
}

export interface TaskStatus {
  id: number;
  name: string;
}

export interface TaskRequest {
  title: string;
  description: string | null;
  statusId: number;
  dueDate: string;
}
