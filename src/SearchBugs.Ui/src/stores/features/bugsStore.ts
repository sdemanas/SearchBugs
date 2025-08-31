import { create } from 'zustand';
import { Bug, BugStatus, BugPriority } from '@/lib/api';

interface BugsState {
  // State - no data/loading states (React Query handles that)
  selectedBug: Bug | null;
  searchQuery: string;
  statusFilter: BugStatus | 'all';
  priorityFilter: BugPriority | 'all';
  assigneeFilter: string | 'all';
  projectFilter: string | 'all';
  sortBy: 'title' | 'status' | 'priority' | 'createdOnUtc' | 'updatedOnUtc';
  sortOrder: 'asc' | 'desc';
  currentPage: number;
  pageSize: number;

  // Actions for state management only
  setSelectedBug: (bug: Bug | null) => void;
  setSearchQuery: (query: string) => void;
  setStatusFilter: (status: BugStatus | 'all') => void;
  setPriorityFilter: (priority: BugPriority | 'all') => void;
  setAssigneeFilter: (assignee: string | 'all') => void;
  setProjectFilter: (projectId: string | 'all') => void;
  setSorting: (sortBy: BugsState['sortBy'], order: 'asc' | 'desc') => void;
  setCurrentPage: (page: number) => void;
  setPageSize: (size: number) => void;
  clearFilters: () => void;

  // Computed values (works with external data)
  filterBugs: (bugs: Bug[]) => Bug[];
  paginateBugs: (bugs: Bug[]) => { bugs: Bug[]; totalPages: number; totalCount: number };
}

export const useBugsStore = create<BugsState>((set, get) => ({
  // State - no data/loading/error states
  selectedBug: null,
  searchQuery: '',
  statusFilter: 'all',
  priorityFilter: 'all',
  assigneeFilter: 'all',
  projectFilter: 'all',
  sortBy: 'createdOnUtc',
  sortOrder: 'desc',
  currentPage: 1,
  pageSize: 10,

  // Actions for state management only
  setSelectedBug: (bug: Bug | null) => {
    set({ selectedBug: bug });
  },

  setSearchQuery: (query: string) => {
    set({ searchQuery: query, currentPage: 1 }); // Reset to first page on search
  },

  setStatusFilter: (status: BugStatus | 'all') => {
    set({ statusFilter: status, currentPage: 1 });
  },

  setPriorityFilter: (priority: BugPriority | 'all') => {
    set({ priorityFilter: priority, currentPage: 1 });
  },

  setAssigneeFilter: (assignee: string | 'all') => {
    set({ assigneeFilter: assignee, currentPage: 1 });
  },

  setProjectFilter: (projectId: string | 'all') => {
    set({ projectFilter: projectId, currentPage: 1 });
  },

  setSorting: (sortBy, order) => {
    set({ sortBy, sortOrder: order });
  },

  setCurrentPage: (page: number) => {
    set({ currentPage: page });
  },

  setPageSize: (size: number) => {
    set({ pageSize: size, currentPage: 1 });
  },

  clearFilters: () => {
    set({
      searchQuery: '',
      statusFilter: 'all',
      priorityFilter: 'all',
      assigneeFilter: 'all',
      projectFilter: 'all',
      sortBy: 'createdOnUtc',
      sortOrder: 'desc',
      currentPage: 1,
    });
  },

  // Computed values that work with React Query data
  filterBugs: (bugs: Bug[]) => {
    const { searchQuery, statusFilter, priorityFilter, assigneeFilter, projectFilter, sortBy, sortOrder } = get();
    let filtered = [...bugs];

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter((bug) =>
        bug.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
        bug.description?.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Filter by status
    if (statusFilter !== 'all') {
      filtered = filtered.filter((bug) => bug.status === statusFilter);
    }

    // Filter by priority
    if (priorityFilter !== 'all') {
      filtered = filtered.filter((bug) => bug.priority === priorityFilter);
    }

    // Filter by assignee
    if (assigneeFilter !== 'all') {
      filtered = filtered.filter((bug) => bug.assigneeId === assigneeFilter);
    }

    // Filter by project
    if (projectFilter !== 'all') {
      filtered = filtered.filter((bug) => bug.projectId === projectFilter);
    }

    // Sort
    filtered.sort((a, b) => {
      let aValue: any;
      let bValue: any;

      if (sortBy === 'createdOnUtc' || sortBy === 'updatedOnUtc') {
        aValue = new Date(a[sortBy]);
        bValue = new Date(b[sortBy] || a[sortBy]);
      } else {
        aValue = a[sortBy];
        bValue = b[sortBy];
      }

      const comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      return sortOrder === 'asc' ? comparison : -comparison;
    });

    return filtered;
  },

  paginateBugs: (bugs: Bug[]) => {
    const { currentPage, pageSize } = get();
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedBugs = bugs.slice(startIndex, endIndex);
    const totalPages = Math.ceil(bugs.length / pageSize);

    return {
      bugs: paginatedBugs,
      totalPages,
      totalCount: bugs.length,
    };
  },
}));
