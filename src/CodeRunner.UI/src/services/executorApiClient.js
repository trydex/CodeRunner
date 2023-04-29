import axios from "axios";
import api from "../api";

const ExecutionStatus = {
  Pending: 0,
  Failed: 1,
  Done: 2
}

export default class ExecutorApiClient {
  constructor() {
    const apiDomain = api.executor.domain;
    
    this.httpClient = axios.create({
        baseURL: apiDomain,
      });
  }

  async executeCode(code, processCount, processResultFunc) {
    if(this._getResultTimeout)
      clearTimeout(this._getResultTimeout);

    const executeUrl = api.executor.endpoints.execute();
      
    let response = await this.httpClient.post(executeUrl, {code, processCount});
    let executionResult = response.data;

    await this._getResult(executionResult.id, processResultFunc);
  }

  async _getResult(id, processResultFunc) {
    this.awaitedResultId = id;
    const getResultUrl = api.executor.endpoints.getResult(id);

    const response = await this.httpClient.get(getResultUrl);
    const executionResult = response.data;
      
    if(executionResult.status == ExecutionStatus.Pending) {
      this._getResultTimeout = setTimeout(() => this._getResult(id, processResultFunc), 3000);
    }
    else if(this.awaitedResultId == id){
      processResultFunc(executionResult);
    }
  }
}