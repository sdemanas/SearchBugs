import { create } from 'zustand';
import { Project } from '@/lib/api';

interface ProjectsState {
  // State - no data/loading states (React Query handles that)
  selectedProject: Project | null;
  searchQuery: string;
  sortBy: 'name' | 'createdOnUtc' | 'updatedOnUtc';
  sortOrder: 'asc' | 'desc';
  
  // Actions for state management only
  setSelectedProject: (project: Project | null) => void;
  setSearchQuery: (query: string) => void;
  setSorting: (sortBy: ProjectsState['sortBy'], order: 'asc' | 'desc') => void;
  clearFilters: () => void;
  
  // Computed values (works with external data)
  filterProjects: (projects: Project[]) => Project[];
}

export const useProjectsStore = create<ProjectsState>((set, get) => ({
  // State - no data/loading/error states
  selectedProject: null,
  searchQuery: '',
  sortBy: 'name',
  sortOrder: 'asc',

  // Actions for state management only
  setSelectedProject: (project: Project | null) => {
    set({ selectedProject: project });
  },

  setSearchQuery: (query: string) => {
    set({ searchQuery: query });
  },

  setSorting: (sortBy, order) => {
    set({ sortBy, sortOrder: order });
  },

  clearFilters: () => {
    set({
      searchQuery: '',
      sortBy: 'name',
      sortOrder: 'asc',
    });
  },

  // Computed values that work with React Query data
  filterProjects: (projects: Project[]) => {
    const { searchQuery, sortBy, sortOrder } = get();
    let filtered = [...projects];

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter((project) =>
        project.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        project.description?.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    // Sort
    filtered.sort((a, b) => {
      let aValue: any;
      let bValue: any;

      if (sortBy === 'createdOnUtc' || sortBy === 'updatedOnUtc') {
        aValue = new Date(a[sortBy]);
        bValue = new Date(b[sortBy]);
      } else {
        aValue = a[sortBy];
        bValue = b[sortBy];
      }

      const comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      return sortOrder === 'asc' ? comparison : -comparison;
    });

    return filtered;
  },
}));
