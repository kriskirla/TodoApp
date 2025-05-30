import { StatusType, PriorityType  } from '../types';

export const StatusTypeLabel: Record<StatusType, string> = {
  [StatusType.NotStarted]: 'Not Started',
  [StatusType.InProgress]: 'In Progress',
  [StatusType.Completed]: 'Completed',
};

export const PriorityTypeLabel: Record<PriorityType, string> = {
  [PriorityType.Low]: 'Low',
  [PriorityType.Medium]: 'Medium',
  [PriorityType.High]: 'High',
  [PriorityType.Critical]: 'Critical',
};
