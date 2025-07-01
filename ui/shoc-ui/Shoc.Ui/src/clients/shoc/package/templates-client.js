import BaseAxiosClient from "@/clients/base-axios-client";

/**
 * The client for "secret" service
 */
export default class TemplatesClient extends BaseAxiosClient {
  
  /**
   * Creates new instance of the client
   *
   * @param {*} config The client configuration
   */
  constructor(config) {
    super("shoc-package", config);
  }

  getAll() {
    const url = this.urlify({
      api: `api/templates`
    });
    
    return this.webClient.get(url);
  }

  getVariants(name) {
    const url = this.urlify({
      api: `api/templates/${name}/variants`
    });
    
    return this.webClient.get(url);
  }

  getVariant(name, variant) {
    const url = this.urlify({
      api: `api/templates/${name}/variants/${variant}`
    });
    
    return this.webClient.get(url);
  }

  getVariantSpec(name, variant) {
    const url = this.urlify({
      api: `api/templates/${name}/variants/${variant}/build-spec`
    });
    
    return this.webClient.get(url);
  }
}