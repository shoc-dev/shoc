import { JobValueType } from "@/domain/job";
import { createContext } from "react";

const JobContext = createContext<JobContextValueType | any>({});

export type JobContextValueType = {
    value: JobValueType,
    initialValue: JobValueType,
    load: () => Promise<any>,
    loading: boolean,
    errors: any[]
}

export default JobContext;