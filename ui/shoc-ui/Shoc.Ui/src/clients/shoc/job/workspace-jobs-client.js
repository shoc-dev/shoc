import BaseAxiosClient from "@/clients/base-axios-client";

/**
 * The client for "job" service
 */
export default class WorkspaceJobsClient extends BaseAxiosClient {

  constructor(config) {
    super("shoc-job", config);
  }

  getBy(token, workspaceId, filter) {
    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs`,
      query: { ...filter }
    });

    return this.webClient.get(url, {
      headers: {
        ...this.authBearer(token)
      }
    });
  }

  getByLocalId(token, workspaceId, localId) {
    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs/by-local-id/${localId}`
    });

    return this.webClient.get(url, {
      headers: {
        ...this.authBearer(token)
      }
    });
  }

  getPermissionsByLocalId(token, workspaceId, localId) {
    const url = this.urlify({
      api: `api/workspaces/${workspaceId}/jobs/by-local-id/${localId}/permissions`
    });

    return this.webClient.get(url, {
      headers: {
        ...this.authBearer(token)
      }
    });
  }
  
}
