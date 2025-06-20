export interface AuthenticateUserRequest {
    email: string;
}

export interface LoginResponse {
    token: string;
    expiration: string;
}

export interface RegistrationRequest {
    email: string;
}

export interface User {
    id: string;
    username: string;
}

export interface TodoList {
    id: string;
    title: string;
    ownerId: string;
    items: TodoItem[];
    sharedWith: TodoListShare[];
}

export interface TodoItem {
    id: string;
    todoListId: string;
    name: string;
    description: string;
    mediaUrl: string;
    mediaType: MediaType;
    dueDate: Date;
    status: StatusType;
    priority: PriorityType;
}

export interface TodoListShare {
    id: string,
    listId: string,
    userId: string,
    permission: Permission
}

export interface ItemForm {
    name: string;
    description: string;
    dueDate: Date;
    status: StatusType;
    priority: PriorityType;
    media?: File | null;
}

export enum MediaType {
    Image = 0,
    Video = 1
}

export enum Permission {
    View = 0,
    Edit = 1
}

export enum AttributeType
{
    Name = 0,
    Description = 1,
    DueDate = 2,
    Status = 3,
    Priority = 4
}

export enum OrderType
{
    Descending = 0,
    Ascending = 1
}

export enum PriorityType {
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

export enum StatusType
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}