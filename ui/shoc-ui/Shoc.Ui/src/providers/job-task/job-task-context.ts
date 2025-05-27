import { JobTaskValueType } from "@/domain/job";
import { createContext } from "react";

const JobTaskContext = createContext<JobTaskContextValueType | any>({});

export type JobTaskContextValueType = {
    value: JobTaskValueType,
    initialValue: JobTaskValueType,
    load: () => Promise<any>,
    loading: boolean,
    errors: any[]
}

export default JobTaskContext;