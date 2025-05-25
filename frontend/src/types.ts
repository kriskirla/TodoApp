export interface LoginOutputDto {
    token: string;
    expiration: string;
}

export interface User {
    id: string;
    username: string;
}

export interface UserOutputDto {
    user: User;
    email: string;
}

export interface TodoListOutputDto {
    list: TodoList;
    items: TodoItem[];
    message: string;
    success: boolean;
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
    description: string;
    mediaUrl: string;
    mediaType: MediaType;
    todoListId: string;
}

export interface TodoListShare {
    id: string,
    listId: string,
    userId: string,
    permission: Permission
}

export interface ItemForm {
    description: string;
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