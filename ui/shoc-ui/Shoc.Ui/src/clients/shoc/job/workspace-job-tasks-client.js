import BaseAxiosClient from "@/clients/base-axios-client";

/**
 * The client for "job" service
 */
export default class WorkspaceJobTasksClient extends BaseAxiosClient {

  constructor(config) {
    super("shoc-job", config);
  }

  getAll(token, workspaceId, jobId){

    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs/${jobId}/tasks`
    });

    return this.webClient.get(url, {
      headers: {
        ...this.authBearer(token)
      }
    });
  }

  getBySequence(token, workspaceId, jobId, sequence){

    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs/${jobId}/tasks/by-sequence/${sequence}`
    });

    return this.webClient.get(url, {
      headers: {
        ...this.authBearer(token)
      }
    });
  }

  getLogsBySequenceUrl(token, workspaceId, jobId, sequence){

    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs/${jobId}/tasks/by-sequence/${sequence}/logs`
    });

    return Promise.resolve({ data: { token, url }})
  }
}