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
import { workspaceNamePattern, workspaceTypes, workspaceTypesMap } from "../../../../well-known/workspace"
import ErrorAlert from "@/components/general/error-alert"
import { useCallback, useState } from "react"
import { useForm } from "react-hook-form"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import SpinnerIcon from "@/components/icons/spinner-icon"
import { rpc } from "@/server-actions/rpc"
import { useRouter } from "next/navigation"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { toast } from "sonner"
import { PlusIcon } from "@radix-ui/react-icons"

export default function WorkspaceAddDialogButton({ className, disabled = false, onSuccess }: { className?: string, disabled?: boolean, onSuccess?: (result: any) => {} }) {

  const intl = useIntl();
  const [open, setOpen] = useState(false);
  const [errors, setErrors] = useState<any[]>([]);
  const [progress, setProgress] = useState(false);

  const formSchema = z.object({
    name: z.string().regex(workspaceNamePattern, intl.formatMessage({ id: 'workspaces.validation.invalidName' })),
    description: z.string().min(2, intl.formatMessage({ id: 'workspaces.validation.invalidDescription' })),
    type: z.custom(type => workspaceTypesMap[type], intl.formatMessage({ id: 'workspaces.validation.invalidType' })),
  });

  const form = useForm({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      type: 'individual'
    },
    shouldUseNativeValidation: false
  })

  const submit = useCallback(async (input: any) => {

    setProgress(true);
    setErrors([]);

    const { data, errors } = await rpc('workspace/user-workspaces/create', { input });

    setProgress(false);

    if (errors) {
      setErrors(errors || []);
      return;
    }

    setOpen(false);
    toast(intl.formatMessage({ id: 'workspaces.messages.created' }))

    if(onSuccess){
      onSuccess(data)
    }

  }, [intl, onSuccess]);

  async function onSubmit(values: any) {
    await submit({
      name: values.name,
      description: values.description,
      type: values.type
    });
  }

  const onOpenChangeWrapper = (openValue: boolean) => {
    if (!openValue) {
      form.reset()
    }
    setOpen(openValue)
  }

  return <Dialog open={open} onOpenChange={onOpenChangeWrapper} modal>
    <DialogTrigger asChild>
      <Button variant="outline" className={className} disabled={disabled}>
        <PlusIcon  />
        <span className="hidden sm:flex">
          {intl.formatMessage({ id: 'workspaces.add' })}
        </span>
      </Button>
    </DialogTrigger>
    <DialogContent className="w-4/5 md:w-1/2">
      <DialogHeader>
        <DialogTitle>{intl.formatMessage({ id: 'workspaces.add' })}</DialogTitle>
        <DialogDescription>
          {intl.formatMessage({ id: 'workspaces.create.notice' })}
        </DialogDescription>
      </DialogHeader>
      <ErrorAlert errors={errors} title={intl.formatMessage({ id: 'workspaces.create.error' })} />
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
                        placeholder={intl.formatMessage({ id: 'workspaces.placeholders.name' })}
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
                    <FormLabel>{intl.formatMessage({ id: 'workspaces.labels.type' })}</FormLabel>
                    <Select onValueChange={fieldNoRef.onChange} {...fieldNoRef}>
                      <FormControl>
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder={intl.formatMessage({ id: 'workspaces.placeholders.type' })} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {workspaceTypes.map((item) => <SelectItem key={item.key} value={item.key}>{intl.formatMessage({ id: item.display })}</SelectItem>)}
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
                        placeholder={intl.formatMessage({ id: 'workspaces.placeholders.description' })}
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