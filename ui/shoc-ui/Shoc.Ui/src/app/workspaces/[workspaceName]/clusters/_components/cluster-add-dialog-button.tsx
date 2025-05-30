"use client"

import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { zodResolver } from "@hookform/resolvers/zod"
import { useIntl } from "react-intl"
import { z } from "zod"
import { clusterNamePattern, clusterTypes, clusterTypesMap } from "./well-known"
import ErrorAlert from "@/components/general/error-alert"
import { useCallback, useState } from "react"
import { useForm } from "react-hook-form"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import SpinnerIcon from "@/components/icons/spinner-icon"
import { rpc } from "@/server-actions/rpc"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { toast } from "sonner"
import { PlusIcon } from "@radix-ui/react-icons"

export default function ClusterAddDialogButton({ workspaceId, className, disabled = false, onSuccess }: { workspaceId: string, className?: string, disabled?: boolean, onSuccess?: (result: any) => any }) {

  const intl = useIntl();
  const [open, setOpen] = useState(false);
  const [errors, setErrors] = useState<any[]>([]);
  const [progress, setProgress] = useState(false);
  const [testing, setTesting] = useState(false);

  const formSchema = z.object({
    name: z.string().regex(clusterNamePattern, intl.formatMessage({ id: 'clusters.validation.invalidName' })),
    description: z.string().min(2, intl.formatMessage({ id: 'clusters.validation.invalidDescription' })),
    type: z.custom(type => clusterTypesMap[type], intl.formatMessage({ id: 'clusters.validation.invalidType' })),
    configuration: z.string().optional()
  });

  const form = useForm({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      type: 'k8s',
      configuration: ''
    },
    shouldUseNativeValidation: false
  })

  const submit = useCallback(async (input: any) => {

    setProgress(true);
    setErrors([]);

    const { data, errors } = await rpc('cluster/workspace-clusters/create', { workspaceId, input });

    setProgress(false);

    if (errors) {
      setErrors(errors || []);
      return;
    }

    form.reset();
    setOpen(false);
    toast(intl.formatMessage({ id: 'clusters.messages.created' }))

    if(onSuccess){
      onSuccess(data)
    }

  }, [intl, form, onSuccess, workspaceId]);

  async function onSubmit(values: any) {
    await submit({
      name: values.name,
      description: values.description,
      type: values.type,
      configuration: values.configuration
    });
  }

  const onOpenChangeWrapper = (openValue: boolean) => {
    if (!openValue) {
      form.reset()
    }
    setOpen(openValue)
  }

  const dryTest = useCallback(async () => {
    const configuration = form.getValues().configuration;

    setTesting(true);
    const { data, errors } = await rpc('cluster/workspace-clusters/ping', { workspaceId, input: { configuration } });
    setTesting(false);

    if (errors || !data) {
      toast.error(intl.formatMessage({ id: 'clusters.messages.testFailed' }))
      return;
    }
    
    toast(intl.formatMessage({ id: 'clusters.messages.testSuccess' }, { nodesCount: data.nodesCount }))
  }, [form, intl, workspaceId]);

  return <Dialog open={open} onOpenChange={onOpenChangeWrapper} modal>
    <DialogTrigger asChild>
      <Button variant="outline" className={className} disabled={disabled}>
        <PlusIcon className="w-4 h-4 mr-2" />
        {intl.formatMessage({ id: 'clusters.add' })}
      </Button>
    </DialogTrigger>
    <DialogContent className="w-4/5 md:w-1/2">
      <DialogHeader>
        <DialogTitle>{intl.formatMessage({ id: 'clusters.add' })}</DialogTitle>
        <DialogDescription>
          {intl.formatMessage({ id: 'clusters.create.notice' })}
        </DialogDescription>
      </DialogHeader>
      <ErrorAlert errors={errors} title={intl.formatMessage({ id: 'clusters.create.error' })} />
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <div className="grid gap-2">
            <div className="grid gap-1">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'global.labels.name' })}</FormLabel>
                    <FormControl>
                      <Input
                        autoFocus
                        placeholder={intl.formatMessage({id: 'clusters.placeholders.name'})}
                        type="text"
                        autoCapitalize="none"
                        autoComplete="off"
                        aria-autocomplete="none"
                        autoCorrect="off"
                        disabled={progress}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <div className="grid grap-1">
              <FormField
                control={form.control}
                name="type"
                render={({ field: { ref, ...fieldNoRef } }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'global.labels.type' })}</FormLabel>
                    <Select disabled={progress} onValueChange={fieldNoRef.onChange} {...fieldNoRef}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder={intl.formatMessage({ id: 'clusters.placeholders.type' })} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {clusterTypes.map((item) => <SelectItem key={item.key} value={item.key}>{intl.formatMessage({ id: item.display })}</SelectItem>)}
                      </SelectContent>
                    </Select>
                  </FormItem>
                )}
              />
            </div>
            <div className="grid gap-1">
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'global.labels.description' })}</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder={intl.formatMessage({ id: 'clusters.placeholders.description' })}
                        disabled={progress}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
            <div className="grid gap-1">
              <FormField
                control={form.control}
                name="configuration"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{intl.formatMessage({ id: 'global.labels.configuration' })}</FormLabel>
                    <FormControl>
                      <Textarea
                        className="min-h-[85px]"
                        placeholder={intl.formatMessage({ id: 'clusters.placeholders.configuration' })}
                        disabled={progress}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" disabled={testing} onClick={() => dryTest()}>
                {testing && (
                  <SpinnerIcon className="mr-2 h-4 w-4 animate-spin" />
                )}
                {intl.formatMessage({ id: 'global.actions.test' })}
              </Button>
              <Button type="submit" disabled={progress}>
                {progress && (
                  <SpinnerIcon className="mr-2 h-4 w-4 animate-spin" />
                )}
                {intl.formatMessage({ id: 'global.actions.create' })}
              </Button>
            </DialogFooter>
          </div>
        </form>
      </Form>
    </DialogContent>
  </Dialog>
}