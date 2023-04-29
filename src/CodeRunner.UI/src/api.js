const api = {
    executor: {
      domain: 'http://localhost:5136',
      endpoints: {
        execute: () => '/execute',
        getResult: (id) => `/execute/${id}`,
      },
    },
  };

export default api;